select
    "Id"        as game_mode_id,
    "Name"      as name,
    "IsSolo"    as is_solo,
    "SoloParty" as solo_party
from {{ source('analog', 'game_mode') }}
