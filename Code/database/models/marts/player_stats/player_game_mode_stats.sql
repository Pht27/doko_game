select
    r.player_id,
    r.game_mode_id,
    gm.name                                                                  as game_mode_name,
    r.party,
    count(*)                                                                 as games,
    count(*) filter (where r.won)                                            as wins,
    round(count(*) filter (where r.won)::decimal / count(*), 3)             as win_rate,
    round(avg(r.game_value), 2)                                             as avg_game_value,
    round(avg(r.points_won_lost), 2)                                        as avg_points_won_lost,
    round(avg(r.point_delta), 2)                                            as avg_points_earned
from {{ ref('int_player_round_results') }}  r
join {{ ref('stg_analog__game_modes') }}    gm on gm.game_mode_id = r.game_mode_id
group by r.player_id, r.game_mode_id, gm.name, r.party
