select
    tm1.player_id,
    tm2.player_id                                                            as partner_id,
    p.name                                                                   as partner_name,
    count(*)                                                                 as games_together,
    count(*) filter (where tr.won)                                           as wins_together,
    round(count(*) filter (where tr.won)::decimal / count(*), 3)            as win_rate_together,
    round(avg(tr.points_won_lost), 2)                                        as avg_points_won_lost
from {{ ref('stg_analog__team_members') }}   tm1
join {{ ref('stg_analog__team_members') }}   tm2 on tm2.team_id   = tm1.team_id
                                                 and tm2.player_id != tm1.player_id
join {{ ref('stg_analog__players') }}        p   on p.player_id   = tm2.player_id
join {{ ref('int_team_round_results') }}     tr  on tr.team_id    = tm1.team_id
group by tm1.player_id, tm2.player_id, p.name
