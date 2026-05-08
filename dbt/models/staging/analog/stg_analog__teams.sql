select
    "Id"      as team_id,
    "RoundId" as round_id,
    "Party"   as party,
    "Name"    as team_name
from {{ source('analog', 'team') }}
