select
    tm.player_id,
    sc.special_card_id,
    sc.name                                                                  as special_card_name,
    tr.party,
    count(*)                                                                 as occurrences,
    count(*) filter (where tr.won)                                           as wins,
    round(count(*) filter (where tr.won)::decimal / count(*), 3)            as win_rate,
    round(avg(tr.game_value), 2)                                            as avg_game_value,
    round(avg(tr.points_won_lost), 2)                                       as avg_points_won_lost
from {{ ref('stg_analog__round_special_cards') }} rsc
join {{ ref('stg_analog__special_cards') }}       sc on sc.special_card_id = rsc.special_card_id
join {{ ref('stg_analog__team_members') }}        tm on tm.team_id         = rsc.team_id
join {{ ref('int_team_round_results') }}          tr on tr.team_id         = rsc.team_id
group by tm.player_id, sc.special_card_id, sc.name, tr.party
