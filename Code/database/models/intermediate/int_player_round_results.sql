with team_sizes as (
    select
        team_id,
        count(*) as team_size
    from {{ ref('stg_analog__team_members') }}
    group by team_id
),

base as (
    select
        tm.player_id,
        t.round_id,
        r.game_mode_id,
        t.party,
        (t.party = r.winning_party)                                          as won,
        ts.team_size,
        (ts.team_size = 1)                                                   as is_alone,
        (gm.is_solo and gm.solo_party = t.party)                             as is_solo,
        case when gm.is_solo and gm.solo_party = t.party then 3 else 1 end  as solo_multiplier,
        case when t.party = r.winning_party
             then r.points::decimal
             else -r.points::decimal
        end                                                                  as game_value
    from {{ ref('stg_analog__team_members') }} tm
    join {{ ref('stg_analog__teams') }}      t  on t.team_id       = tm.team_id
    join {{ ref('stg_analog__rounds') }}     r  on r.round_id      = t.round_id
    join {{ ref('stg_analog__game_modes') }} gm on gm.game_mode_id = r.game_mode_id
    join team_sizes                          ts on ts.team_id       = tm.team_id
)

select
    player_id,
    round_id,
    game_mode_id,
    party,
    won,
    team_size,
    is_alone,
    is_solo,
    solo_multiplier,
    game_value,
    game_value * solo_multiplier                         as points_won_lost,
    game_value * solo_multiplier / team_size             as point_delta
from base
