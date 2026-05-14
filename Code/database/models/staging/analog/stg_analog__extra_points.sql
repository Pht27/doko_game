select
    "Id"   as extra_point_id,
    "Name" as name
from {{ source('analog', 'extra_point') }}
