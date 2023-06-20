 
# # Get Name of activity -> qdbus org.kde.ActivityManager /ActivityManager/Activities ActivityName *$ID*
# $(xdotool getwindowfocus)

# wmctrl -l  output -> list of window ids
# qdbus org.kde.ActivityManager /ActivityManager/Activities ListActivities  output -> list of activity ids
# xprop -id XID _KDE_NET_WM_ACTIVITIES  output -> _KDE_NET_WM_ACTIVITIES(STRING) = "1954e667-b011-49d1-873d-8e7f085f524d"
# xprop -id XID _NET_WM_DESKTOP | awk '{print $3}'  output -> 0
# xprop -id XID WM_CLASS | awk '{print $3}'  output -> "cool-retro-term",
# xprop -id $(xdotool getwindowfocus) WM_NAME | awk '{print $3}'  output ->  "nero_admin@fedora:~"
# xdotool XID getwindowgeometry  output -> Position: 2781,162 (screen: 0)  Geometry: 1694x1384
# xdotool getwindowfocus getwindowgeometry | grep Position: | awk '{print $2}'  output ->  14,90
# xrandr --listactivemonitors

# xdpyinfo | awk '/dimensions/{print $2}'
# 4490x1680

# xrandr --current | grep '*' | uniq | awk '{print $1}'
# 1680x1050
# 3440x1440

# xrandr | grep ' connected' | awk '{print $4}'
# left
# 3440x1440+1050+120

# wmctrl -l | awk '{print $1}' | xargs -I % xdotool -id % getwindowgeometry | grep Position: | awk '{print $2}'

# xwininfo -id $(xdotool getwindowfocus) | grep Absolute | awk '{print $4}'
# wmctrl -l | awk '{print $1}' | xargs -I % xwininfo -id % | grep -E 'Absolute|Window id:' | awk '{print $4}'
