import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import keycloak from './auth/keycloak.ts'

const root = createRoot(document.getElementById('root')!)

async function startApp() {
  try {
    const authenticated = await keycloak.init({
      onLoad: 'login-required',
      pkceMethod: 'S256',
    })

    if (authenticated) {
      root.render(
        <StrictMode>
        <App />
        </StrictMode>,
      )
    } else {
      root.render(
        <p role="alert">Sign-in did not complete. Reload to try again.</p>
      )
    }
  } catch {
    root.render(
        <p role="alert">Sign-in did not complete. Reload to try again.</p>
    )
  }
}

void startApp()
