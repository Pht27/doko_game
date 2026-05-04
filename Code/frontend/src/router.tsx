import { createBrowserRouter } from 'react-router-dom';
import { AppLayout } from '@/layouts/AppLayout';
import { GamingLayout } from '@/layouts/GamingLayout';
import { LandingPage } from '@/pages/LandingPage/LandingPage';
import { RulesPage } from '@/pages/RulesPage/RulesPage';
import { LobbyPage } from '@/pages/LobbyPage/LobbyPage';
import { GamePage } from '@/pages/GamePage/GamePage';
import { HotSeatPage } from '@/pages/HotSeatPage/HotSeatPage';
import { AnalogPlayersPage } from '@/pages/analog/AnalogPlayersPage';
import { AnalogPlayerPage } from '@/pages/analog/AnalogPlayerPage';
import { AnalogHistoryPage } from '@/pages/analog/AnalogHistoryPage';
import { AnalogNewRoundPage } from '@/pages/analog/AnalogNewRoundPage';
import { AnalogEditRoundPage } from '@/pages/analog/AnalogEditRoundPage';
import { Root } from './Root';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <Root />,
    children: [
      {
        element: <AppLayout />,
        children: [
          { index: true, element: <LandingPage /> },
          { path: 'rules', element: <RulesPage /> },
          { path: 'analog/players', element: <AnalogPlayersPage /> },
          { path: 'analog/players/:id', element: <AnalogPlayerPage /> },
          { path: 'analog/history', element: <AnalogHistoryPage /> },
          { path: 'analog/new', element: <AnalogNewRoundPage /> },
          { path: 'analog/edit/:id', element: <AnalogEditRoundPage /> },
        ],
      },
      {
        element: <GamingLayout />,
        children: [
          { path: 'lobby', element: <LobbyPage /> },
          { path: 'game/:id', element: <GamePage /> },
          { path: 'hot-seat', element: <HotSeatPage /> },
        ],
      },
    ],
  },
]);
