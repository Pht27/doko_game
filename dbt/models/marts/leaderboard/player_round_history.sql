select
    r.player_id,
    rnd.round_id,
    rnd.played_at,
    r.point_delta,
    r.won,
    round(
        p.starting_points + sum(r.point_delta) over (
            partition by r.player_id
            order by rnd.played_at, rnd.round_id
            rows between unbounded preceding and current row
        ),
        2
    ) as cumulative_points
from {{ ref('int_player_round_results') }}  r
join {{ ref('stg_analog__rounds') }}        rnd on rnd.round_id  = r.round_id
join {{ ref('stg_analog__players') }}       p   on p.player_id   = r.player_id
