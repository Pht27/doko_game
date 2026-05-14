select
    sc.special_card_id,
    sc.name,
    count(*)                                                                 as occurrences,
    count(*) filter (where tr.won)                                           as wins,
    round(count(*) filter (where tr.won)::decimal / count(*), 3)            as win_rate,
    round(avg(tr.game_value), 2)                                            as avg_game_value,
    round(avg(tr.points_won_lost), 2)                                       as avg_points_won_lost
from {{ ref('stg_analog__round_special_cards') }} rsc
join {{ ref('stg_analog__special_cards') }}       sc on sc.special_card_id = rsc.special_card_id
join {{ ref('int_team_round_results') }}          tr on tr.team_id         = rsc.team_id
group by sc.special_card_id, sc.name
