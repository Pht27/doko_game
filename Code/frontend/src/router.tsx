import { createBrowserRouter } from 'react-router-dom';
import { AppLayout } from '@/layouts/AppLayout';
import { GamingLayout } from '@/layouts/GamingLayout';
import { LandingPage } from '@/pages/LandingPage/LandingPage';
import { RulesPage } from '@/pages/RulesPage/RulesPage';
import { LobbyPage } from '@/pages/LobbyPage/LobbyPage';
import { GamePage } from '@/pages/GamePage/GamePage';
import { HotSeatPage } from '@/pages/HotSeatPage/HotSeatPage';
import { PlayersPage } from '@/pages/PlayersPage/PlayersPage';
import { PlayerPage } from '@/pages/PlayerPage/PlayerPage';
import { HistoryPage } from '@/pages/HistoryPage/HistoryPage';
import { AnalogNewRoundPage } from '@/pages/AnalogNewRoundPage/AnalogNewRoundPage';
import { AnalogEditRoundPage } from '@/pages/AnalogEditRoundPage/AnalogEditRoundPage';
import { LeaderboardPage } from '@/pages/LeaderboardPage/LeaderboardPage';
import { NotFoundPage } from '@/pages/NotFoundPage/NotFoundPage';
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
          { path: 'players', element: <PlayersPage /> },
          { path: 'players/:id', element: <PlayerPage /> },
          { path: 'history', element: <HistoryPage /> },
          { path: 'analog/new', element: <AnalogNewRoundPage /> },
          { path: 'analog/edit/:id', element: <AnalogEditRoundPage /> },
          { path: 'leaderboard', element: <LeaderboardPage /> },
          { path: '*', element: <NotFoundPage /> },
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
