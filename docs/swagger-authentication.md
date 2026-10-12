# Swagger sign-in recovery notes

Use these notes to recreate Swagger sign-in after moving to a new development
machine or rebuilding the local Keycloak realm. They record the configuration
used by this project. Keycloak stores its clients, users, and role assignments
outside the repository; cloning the code does not recreate those settings.

## Starting point

These steps assume the API and Keycloak already run locally:

| Setting | Value |
| --- | --- |
| Keycloak | `http://localhost:8081` |
| Realm | `opsportal` |
| API configuration: `Api:Authority` | `http://localhost:8081/realms/opsportal` |
| API configuration: `Api:Audience` | `opsportal-api` |
| Swagger, HTTP launch profile | `http://localhost:5096/swagger` |
| Swagger, HTTPS launch profile | `https://localhost:7051/swagger` |

Restore the two API configuration values through the API project's .NET User
Secrets. Swagger reads `Api:Authority` to build its Keycloak endpoint URLs and
is available only in the Development environment.

If the realm was lost, create `opsportal` with realm roles `Reader` and `Admin`.
Create local test users `opsreader` and `opsadmin`, set their passwords locally,
and assign the matching realm role to each user. Existing restored users and
roles can be reused. The [role mapping](authorization-role-mapping.md) records
what each role permits.

## Recreate the Swagger client

1. Sign in to the Keycloak admin console and select the `opsportal` realm.
2. Open **Clients → Create client**. Choose **OpenID Connect**, use client ID
   `opsportal-swagger`, and name it `OpsPortal Swagger`.
3. Set **Client authentication** and **Authorization** to **Off**. Enable
   **Standard flow**; disable **Direct access grants**, **Implicit flow**, and
   other authentication flows. This is a public browser client with no client
   secret.
4. Add both **Valid redirect URIs**, one entry per line:

   ```text
   http://localhost:5096/swagger/oauth2-redirect.html
   https://localhost:7051/swagger/oauth2-redirect.html
   ```

5. Add both **Web origins**, one entry per line:

   ```text
   http://localhost:5096
   https://localhost:7051
   ```

6. Leave the other URL fields blank and save.
7. In the client's **Advanced** tab, set **Proof Key for Code Exchange Code
   Challenge Method** to **S256** and save.

The redirect URIs return the sign-in result to Swagger. The web origins allow
Swagger to call Keycloak's token endpoint. Both API launch profiles are listed
so either local Swagger URL works.

## Add the API audience

The Swagger client requests tokens; `opsportal-api` is the API those tokens
are intended for. Configure that audience on the new client:

1. Open **Client scopes → opsportal-swagger-dedicated → Mappers**.
2. Choose **Configure a new mapper**, or **Add mapper → By configuration** if
   mappers already exist, then select **Audience**.
3. Name the mapper `opsportal-api-audience`.
4. Leave **Included Client Audience** blank. Set **Included Custom Audience**
   to `opsportal-api`, enable **Add to access token**, disable **Add to ID
   token**, and save.
5. Return to the client's **Client scopes → Evaluate**, select a test user,
   and view **Generated access token**. Confirm `aud` includes `opsportal-api`
   and `realm_access.roles` includes the user's `Reader` or `Admin` role.

The dedicated scope applies the audience mapper to this client's sign-ins.

## Check sign-in

1. Start the API in Development and open Swagger using one of the URLs above.
2. Click **Authorize** and use the **Keycloak** option. The client ID should
   be `opsportal-swagger`, with `openid` selected. Leave the client secret blank.
3. Click **Authorize** and sign in with a local test user. An existing Keycloak
   session may sign in immediately.
4. Close the dialog and execute `GET /api/work-items`. Reader and Admin users
   should receive `200`, including when the collection is empty.

Swagger obtains the token without copying it from browser DevTools. The API
still enforces the [endpoint permissions](authorization-role-mapping.md).
To switch test users, log out of Swagger's Keycloak authorization and end the
existing Keycloak user session before authorizing again; a private browser
window is also useful for checking a different user.

## If recovery does not work

| Symptom | Check |
| --- | --- |
| Only the Bearer token field appears | Run the current API code in Development and refresh Swagger. |
| Keycloak rejects the redirect URI | Match the browser's scheme and port to the exact registered callback URL. |
| A PKCE error appears | The client requires S256 and `UseSwaggerUI` calls `OAuthUsePkce()`. |
| The token exchange is blocked by CORS | Register the Swagger page's origin under the client's Web origins. |
| A request returns `401` after sign-in | Check `Api:Authority`, `Api:Audience`, the token's `aud`, and token expiration; authorize again after configuration changes. |
| A request returns `403` | Check the user's realm roles and the endpoint's policy; obtain a fresh token after changing roles. |

Configuration references: [Keycloak OIDC clients](https://www.keycloak.org/docs/latest/server_admin/index.html#_oidc_clients)
and [Swagger UI OAuth configuration](https://swagger.io/docs/open-source-tools/swagger-ui/usage/oauth2/).
