#!/usr/bin/env bash

PLIST="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.iOS/Info.plist"
APPSETTINGS="$BUILD_REPOSITORY_LOCALPATH/src/Inspector/appsettings.json"

/usr/libexec/PlistBuddy -c "Set :CFBundleShortVersionString 1.0.${APPCENTER_BUILD_ID}" $PLIST

cat $PLIST
echo "Updated info.plist!"

