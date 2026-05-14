select
    gm.game_mode_id,
    gm.name                                                                  as game_mode_name,
    count(r.round_id)                                                        as total_rounds,
    round(avg(r.points::decimal), 2)                                         as avg_game_value,
    case when count(r.round_id) > 0
         then round(
             count(r.round_id) filter (where r.winning_party = 0)::decimal
             / count(r.round_id), 3)
         else null end                                                        as re_win_rate,
    case when count(r.round_id) > 0
         then round(avg(
             case when r.winning_party = 0
                  then  r.points::decimal
                  else -r.points::decimal
             end), 2)
         else null end                                                        as re_avg_game_value
from {{ ref('stg_analog__game_modes') }}  gm
left join {{ ref('stg_analog__rounds') }} r on r.game_mode_id = gm.game_mode_id
group by gm.game_mode_id, gm.name
