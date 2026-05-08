select
    p.player_id,
    p.name,
    p.is_active,
    round(p.starting_points + coalesce(sum(r.point_delta), 0), 2)             as total_points,
    count(r.round_id)                                                          as games_played,
    count(r.round_id) filter (where r.won)                                    as wins,
    count(r.round_id) filter (where not r.won)                                as losses,
    case
        when count(r.round_id) > 0
        then round(
            count(r.round_id) filter (where r.won)::decimal / count(r.round_id),
            3
        )
        else 0
    end                                                                        as win_rate,
    case
        when count(r.round_id) > 0
        then round(sum(r.point_delta) / count(r.round_id), 2)
        else 0
    end                                                                        as avg_points_per_game
from {{ ref('stg_analog__players') }}       p
left join {{ ref('int_player_round_results') }} r on r.player_id = p.player_id
group by p.player_id, p.name, p.is_active, p.starting_points
