def trimmed:
  sub("^[\\s\\x{FEFF}]+"; "") | sub("[\\s\\x{FEFF}]+$"; "");

def unwrapped_reply:
  trimmed
  | sub("^```[A-Za-z]*\\s*"; "")
  | sub("```\\s*$"; "")
  | trimmed;

def without_trailing_commas:
  explode
  | reduce .[] as $c (
      { out: [], in_string: false, escaped: false, pending: null, previous: null };
      if .in_string then
        .out += [$c]
        | if .escaped then .escaped = false
          elif $c == 92 then .escaped = true
          elif $c == 34 then .in_string = false
          else . end
      elif $c == 34 then
        .out += [$c] | .in_string = true | .pending = null | .previous = $c
      elif $c == 32 or $c == 9 or $c == 10 or $c == 13 then
        .out += [$c]
      else
        (if ($c == 125 or $c == 93) and .pending != null then .out[.pending] = 32 else . end)
        | .previous as $previous
        | .pending = (if $c == 44 and ([123, 91, 44, 58] | index([$previous]) == null) then (.out | length) else null end)
        | .previous = $c
        | .out += [$c]
      end
    )
  | .out
  | implode;

def repaired_reply:
  unwrapped_reply | without_trailing_commas;
