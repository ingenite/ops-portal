import Keycloak from "keycloak-js";

const keycloak = new Keycloak({
  url: "http://localhost:8081",
  realm: "opsportal",
  clientId: "opsportal-web",
});

export default keycloak;
