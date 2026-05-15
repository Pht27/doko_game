select
    ep.extra_point_id,
    ep.name,
    tr.party,
    count(*)                                                                 as occurrences,
    sum(rep.count)                                                           as total_count,
    count(*) filter (where tr.won)                                           as wins,
    round(count(*) filter (where tr.won)::decimal / count(*), 3)            as win_rate,
    round(avg(tr.game_value), 2)                                            as avg_game_value,
    round(avg(tr.points_won_lost), 2)                                       as avg_points_won_lost
from {{ ref('stg_analog__round_extra_points') }} rep
join {{ ref('stg_analog__extra_points') }}       ep on ep.extra_point_id = rep.extra_point_id
join {{ ref('int_team_round_results') }}         tr on tr.team_id        = rep.team_id
group by ep.extra_point_id, ep.name, tr.party
