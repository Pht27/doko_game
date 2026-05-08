select
    "RoundId"      as round_id,
    "ExtraPointId" as extra_point_id,
    "TeamId"       as team_id,
    "Count"        as count
from {{ source('analog', 'round_extra_point') }}
