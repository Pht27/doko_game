select
    "Id"           as round_id,
    "PlayedAt"     as played_at,
    "WinningParty" as winning_party,
    "Points"       as points,
    "GameModeId"   as game_mode_id
from {{ source('analog', 'round') }}
