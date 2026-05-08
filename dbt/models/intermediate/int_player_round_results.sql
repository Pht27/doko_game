with team_sizes as (
    select
        team_id,
        count(*) as team_size
    from {{ ref('stg_analog__team_members') }}
    group by team_id
)

select
    tm.player_id,
    t.round_id,
    (t.party = r.winning_party)                                          as won,
    ts.team_size,
    case when gm.is_solo and gm.solo_party = t.party then 3 else 1 end  as solo_multiplier,
    case
        when t.party = r.winning_party then
             case when gm.is_solo and gm.solo_party = t.party then 3 else 1 end
             * r.points::decimal / ts.team_size
        else
            -case when gm.is_solo and gm.solo_party = t.party then 3 else 1 end
             * r.points::decimal / ts.team_size
    end                                                                  as point_delta
from {{ ref('stg_analog__team_members') }} tm
join {{ ref('stg_analog__teams') }}      t  on t.team_id      = tm.team_id
join {{ ref('stg_analog__rounds') }}     r  on r.round_id     = t.round_id
join {{ ref('stg_analog__game_modes') }} gm on gm.game_mode_id = r.game_mode_id
join team_sizes                          ts on ts.team_id      = tm.team_id
