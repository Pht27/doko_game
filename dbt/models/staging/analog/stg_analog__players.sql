select
    "Id"             as player_id,
    "Name"           as name,
    "IsActive"       as is_active,
    "StartingPoints" as starting_points,
    "CreatedAt"      as created_at
from {{ source('analog', 'player') }}
