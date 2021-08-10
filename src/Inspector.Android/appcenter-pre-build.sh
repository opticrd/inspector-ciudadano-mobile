#!/usr/bin/env bash

SPLASH_ACTIVITY="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/SplashActivity.cs"
MANIFEST="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifest.xml"
MANIFEST_AGENT ="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifestAgent.xml"
APPSETTINGS="$BUILD_REPOSITORY_LOCALPATH/src/Inspector/appsettings.json"
VERSIONNAME=`grep versionName ${MANIFEST} | sed 's/.*versionName\s*=\s*\"\([^\"]*\)\".*/\1/g'`

sed -i.bak "s/android:versionName="\"${VERSIONNAME}\""/android:versionName="\"1.0.${APPCENTER_BUILD_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak
sed -i.bak "s/android:versionCode="\"1\""/android:versionCode="\"${APPCENTER_BUILD_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak

sed -i.bak "s/package="\"do.gob.ogtic.inspector\""/package="\"${APP_BUNDLE_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak

# Print out file for reference
cat $MANIFEST
echo "Updated manifest!"

sed -i.bak "s/android:versionName="\"${VERSIONNAME}\""/android:versionName="\"1.0.${APPCENTER_BUILD_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak
sed -i.bak "s/android:versionCode="\"1\""/android:versionCode="\"${APPCENTER_BUILD_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak

sed -i.bak "s/package="\"do.gob.ogtic.reportero\""/package="\"${APP_BUNDLE_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak


# Print out file for reference
cat $MANIFEST_AGENT
echo "Updated manifest!"



sed -i.bak "s/Label="\"Inspector\""/Label="\"${APP_NAME}\""/" ${SPLASH_ACTIVITY}
rm -f ${SPLASH_ACTIVITY}.bak

cat $SPLASH_ACTIVITY
echo "Updated splash activity!"
