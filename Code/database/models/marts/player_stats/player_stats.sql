select
    p.player_id,
    p.name,
    p.is_active,

    -- total
    count(r.round_id)                                                                    as total_games,
    count(r.round_id) filter (where r.won)                                               as total_wins,
    case when count(r.round_id) > 0
         then round(count(r.round_id) filter (where r.won)::decimal / count(r.round_id), 3)
         else 0 end                                                                      as total_win_rate,
    case when count(r.round_id) > 0
         then round(avg(r.game_value), 2)
         else 0 end                                                                      as total_avg_game_value,
    case when count(r.round_id) > 0
         then round(avg(r.points_won_lost), 2)
         else 0 end                                                                      as total_avg_points_won_lost,
    case when count(r.round_id) > 0
         then round(avg(r.point_delta), 2)
         else 0 end                                                                      as total_avg_points_earned,

    -- solo (player was the solo party)
    count(r.round_id) filter (where r.is_solo)                                           as solo_games,
    count(r.round_id) filter (where r.is_solo and r.won)                                 as solo_wins,
    case when count(r.round_id) filter (where r.is_solo) > 0
         then round(
             count(r.round_id) filter (where r.is_solo and r.won)::decimal
             / count(r.round_id) filter (where r.is_solo), 3)
         else 0 end                                                                      as solo_win_rate,
    case when count(r.round_id) filter (where r.is_solo) > 0
         then round(avg(r.game_value) filter (where r.is_solo), 2)
         else 0 end                                                                      as solo_avg_game_value,
    case when count(r.round_id) filter (where r.is_solo) > 0
         then round(avg(r.points_won_lost) filter (where r.is_solo), 2)
         else 0 end                                                                      as solo_avg_points_won_lost,

    -- alone (team_size = 1, i.e. solo player without solo game mode — Hochzeit, Pflicht etc.)
    count(r.round_id) filter (where r.is_alone)                                          as alone_games,
    count(r.round_id) filter (where r.is_alone and r.won)                                as alone_wins,
    case when count(r.round_id) filter (where r.is_alone) > 0
         then round(
             count(r.round_id) filter (where r.is_alone and r.won)::decimal
             / count(r.round_id) filter (where r.is_alone), 3)
         else 0 end                                                                      as alone_win_rate,
    case when count(r.round_id) filter (where r.is_alone) > 0
         then round(avg(r.point_delta) filter (where r.is_alone), 2)
         else 0 end                                                                      as alone_avg_points_earned

from {{ ref('stg_analog__players') }}           p
left join {{ ref('int_player_round_results') }} r on r.player_id = p.player_id
group by p.player_id, p.name, p.is_active
