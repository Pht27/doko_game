select
    "Id"   as special_card_id,
    "Name" as name
from {{ source('analog', 'special_card') }}
