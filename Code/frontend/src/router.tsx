import { createBrowserRouter } from 'react-router-dom';
import { AppLayout } from '@/layouts/AppLayout';
import { GamingLayout } from '@/layouts/GamingLayout';
import { LandingPage } from '@/pages/LandingPage/LandingPage';
import { RulesPage } from '@/pages/RulesPage/RulesPage';
import { LobbyPage } from '@/pages/LobbyPage/LobbyPage';
import { GamePage } from '@/pages/GamePage/GamePage';
import { HotSeatPage } from '@/pages/HotSeatPage/HotSeatPage';
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
