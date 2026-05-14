select
    "RoundId"       as round_id,
    "SpecialCardId" as special_card_id,
    "TeamId"        as team_id
from {{ source('analog', 'round_special_card') }}
