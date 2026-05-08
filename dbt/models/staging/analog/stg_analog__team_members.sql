select
    "TeamId"   as team_id,
    "PlayerId" as player_id,
    "Position" as position
from {{ source('analog', 'team_member') }}
