import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './styles/index.css'
import App from './App.tsx'
import { PlayerNamesProvider } from './context/PlayerNamesContext.tsx'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <PlayerNamesProvider>
      <App />
    </PlayerNamesProvider>
  </StrictMode>,
)
