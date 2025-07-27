using Newtonsoft.Json.Linq;
using OtpNet;
using ShomreiTorah.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace ShomreiTorah.Schedules.Export {
	public class ShulCloudExporter : IDisposable {
		static readonly Regex findSccsrf = new Regex(@"<input type=""hidden"" name=""sccsrf"" value=""(\w+)""/>");
		static readonly Regex findProductSessionId = new Regex(@"product_session_id:\s*['""](\S+)['""]");
		static readonly Regex findAuthenticateUrl = new Regex(@"https://auth-md.togetherwork.com/auth/realms/shulcloud/login-actions/authenticate[^\s'""]+");


		private readonly CookieContainer cookies = new CookieContainer();
		private readonly HttpClientHandler httpHandler;
		private readonly HttpClient httpClient;
		private readonly string calendarId = Config.ReadAttribute("Schedules", "ShulCloud", "CalendarId");

		public ShulCloudExporter() {
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			httpHandler = new HttpClientHandler { CookieContainer = cookies };
			httpClient = new HttpClient(httpHandler) {
				BaseAddress = new Uri(Config.ReadAttribute("Schedules", "ShulCloud", "BaseAddress"))
			};
		}

		public async Task ExportRange(IProgressReporter progress, ScheduleContext context, DateTime startDate, int weeks) {
			progress.CanCancel = true;
			progress.Caption = "Logging in";
			await Login().ConfigureAwait(false);

			if (progress.WasCanceled) return;
			progress.Caption = "Deleting existing events";

			DateTime endDate = startDate.AddDays(7 * weeks);
			await DeleteAllEvents(startDate, endDate);

			progress.Caption = "Creating events";
			progress.Maximum = weeks * 7;
			for (var date = startDate; date < endDate; date += TimeSpan.FromDays(1)) {
				if (progress.WasCanceled) return;
				var cell = context.GetCell(date);
				await Task.WhenAll(cell.Times.Select(t => CreateEvent(t.Name, t.SqlTime))).ConfigureAwait(false);
				progress.Progress++;
			}
		}

		private async Task Login() {
			// Step 1: Submit credentials to start the 2FA process
			var loginResponse = await httpClient.PostAsync("/login.php", new FormUrlEncodedContent(new Dictionary<string, string> {
				{"sccsrf", await GetCsrfToken()},
				{"action", "login"},
				{"email", Config.ReadAttribute("Schedules", "ShulCloud", "Login")},
				{"password", Config.ReadAttribute("Schedules", "ShulCloud", "Password")},
			})).ConfigureAwait(false);
			var loginPage = await loginResponse.Content.ReadAsStringAsync();
			// <div class='error'>Invalid email or password. Please try again.</div>
			if (loginPage.Contains("Invalid email or password")) throw new InvalidOperationException("ShulCloud login failed: Invalid email or password");

			// Step 2: Extract the session ID from the JS response that starts Keycloak.
			var productSessionId = findProductSessionId.Match(loginPage).Groups[1].Value;
			var redirectUri = $"https://shulcloud.com/callback/mfasuccess.php?product_session_id={productSessionId}";
			//loginPage.Dump();

			// Step 3: Start Keycloak OpenID flow with the session ID.
			Guid state = Guid.NewGuid(), nonce = Guid.NewGuid();
			string openIdAuthUrl = $"https://auth-md.togetherwork.com/auth/realms/shulcloud/protocol/openid-connect/auth?client_id=mfa&redirect_uri={WebUtility.UrlEncode(redirectUri)}&state={state}&response_mode=query&response_type=code&scope=openid&nonce={nonce}";
			var authResponse = await httpClient.GetStringAsync(openIdAuthUrl);

			// Find the <form> POST URL to submit the OTP to.
			var authenticateUrl = WebUtility.HtmlDecode(findAuthenticateUrl.Match(authResponse).Value);

			// Step 4: Post the OTP code to the URL found in the OTP form response, which is stored on the server.
			await httpClient.PostAsync(authenticateUrl, new FormUrlEncodedContent(new Dictionary<string, string> {
				{"otp", new Totp(Base32Encoding.ToBytes(Config.ReadAttribute("Schedules", "ShulCloud", "TotpSecret").Replace(" ", ""))).ComputeTotp()},
				{"login", "Continue"},
			})).ConfigureAwait(false);

			// Step 5: Grab a code to pass to get the OpenId token.
			string openIdCode;
			using (var nonRedirectingHandler = new HttpClientHandler { CookieContainer = cookies, AllowAutoRedirect = false })
			using (var nonRedirectingHttpClient = new HttpClient(nonRedirectingHandler)) {
				var openIdAuthResponse = await nonRedirectingHttpClient.GetAsync(openIdAuthUrl);
				// Grab the code from the redirect target URL.
				openIdCode = HttpUtility.ParseQueryString(openIdAuthResponse.Headers.Location.Query)["code"];
			}

			// Step 6: Grab the full access token to pass back to ShulCloud login
			var authTokenResponse = await httpClient.PostAsync("https://auth-md.togetherwork.com/auth/realms/shulcloud/protocol/openid-connect/token",
				new FormUrlEncodedContent(new Dictionary<string, string> {
					{"code", openIdCode},
					{"grant_type", "authorization_code"},
					{"client_id", "mfa"},
					{"redirect_uri", redirectUri}
				})).ConfigureAwait(false);
			var authTokenJson = JObject.Parse(await authTokenResponse.Content.ReadAsStringAsync());

			// Step 7: Pass that token to some kind of validation callback 
			// This happens in the JS in mfasucces.php before redirecting to /login.php.
			// Removing this breaks the login.
			await httpClient.PostAsync("https://shulcloud.com/callback/validatemfatoken.php", new ByteArrayContent(Encoding.UTF8.GetBytes(new JObject {
				new JProperty("product_session_id", productSessionId),
				new JProperty("authorization_token", authTokenJson.Property("access_token").Value),
			}.ToString(Newtonsoft.Json.Formatting.None))) { Headers = { ContentType = new MediaTypeHeaderValue("application/json") } });


			// Step 8: Pass that token back to /login.php to actually get an auth cookie!
			await httpClient.GetStringAsync("/login.php?authorization_token=" + WebUtility.UrlEncode((string)authTokenJson.Property("access_token").Value));
		}

		static readonly Regex findEventId = new Regex(@"dashboard/event_setup.php\?id=(\d+)");
		private async Task DeleteAllEvents(DateTime from, DateTime to) {
			var allEvents = await httpClient.GetStringAsync(
				$"/dashboard/event_home.php?calendar_ids%5B%5D={calendarId}"
				+ $"&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}&setPerPage=20000"
			).ConfigureAwait(false);
			await Task.WhenAll(findEventId.Matches(allEvents).OfType<Match>().Select(m => m.Groups[1].Value).Select(this.DeleteEvent));
		}

		private Task DeleteEvent(string id) {
			return SendRequest("/dashboard/event_setup.php", new FormUrlEncodedContent(new Dictionary<string, string> {
				{"action", "delete"},
				{"id", id},
				{"confirm", "Y"},
			}));
		}

		async Task CreateEvent(string name, DateTime time) {
			await SendRequest("/dashboard/event_setup.php", new FormUrlEncodedContent(new Dictionary<string, string> {
				{"sccsrf", await GetCsrfToken()},
				{"action", "save"},
				{"info[calendar_id]", calendarId},
				{"clone_id", "0"},
				{"info[is_active]", "Y"},
				{"info[show_rightbar]", "N"},
				{"info[show_on_homepage]", "N"},
				{"info[hide_fixed_position_message]", "Y"},
				{"info[show_on_upcoming_events_widget]", "N"},
				{"info[status]", "confirmed"},
				{"info[privacy]", "public"},
				{"info[name]", name},
				{"dates", new JObject(
					new JProperty("rows", new JArray(
						new JObject(
							new JProperty("id", 0),
							new JProperty("start", time.ToString("yyyy-MM-dd HH:mm:ss")),
							new JProperty("end", time.ToString("yyyy-MM-dd HH:mm:ss"))
						)
					))
				).ToString(Newtonsoft.Json.Formatting.None)},
			}));
		}

		async Task<string> SendRequest(string path, HttpContent content) {
			var responseMessage = await httpClient.PostAsync(path, content);
			string response = await responseMessage.Content.ReadAsStringAsync();
			if (!response.StartsWith("<!DOCTYPE html")) throw new Exception($"Error from {path}:\n{response}");
			return response;
		}

		async Task<string> GetCsrfToken() {
			var responseMessage = await httpClient.GetAsync("/");
			string response = await responseMessage.Content.ReadAsStringAsync();
			return findSccsrf.Match(response).Groups[1].Value;
		}


		///<summary>Releases all resources used by the WordBinder.</summary>
		public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
		///<summary>Releases the unmanaged resources used by the WordBinder and optionally releases the managed resources.</summary>
		///<param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing) {
			if (disposing) {
				this.httpClient.Dispose();
				this.httpHandler.Dispose();
			}
		}
	}
}
