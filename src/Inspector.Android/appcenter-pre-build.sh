#!/usr/bin/env bash

SPLASH_ACTIVITY="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/SplashActivity.cs"
MANIFEST="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifest.xml"
MANIFEST_AGENT="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifestAgent.xml"
APPSETTINGS="$BUILD_REPOSITORY_LOCALPATH/src/Inspector/appsettings.json"
VERSIONNAME=`grep versionName ${MANIFEST} | sed 's/.*versionName\s*=\s*\"\([^\"]*\)\".*/\1/g'`

# Lets work with the manifest files
if [ "$APPCENTER_XAMARIN_CONFIGURATION" != "Release-agent" ]
then

echo "Updating manifest file for agent..."
sed -i.bak "s/android:versionName="\"${VERSIONNAME}\""/android:versionName="\"${BASE_VERSION}.${APPCENTER_BUILD_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak
sed -i.bak "s/android:versionCode="\"1\""/android:versionCode="\"${APPCENTER_BUILD_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak

sed -i.bak "s/package="\"do.gob.ogtic.inspector\""/package="\"${APP_BUNDLE_ID}\""/" ${MANIFEST_AGENT}
rm -f ${MANIFEST_AGENT}.bak

# Print out file for reference
cat $MANIFEST_AGENT
echo "Updated manifest file for agent!"

else

echo "Updating manifest file..."
sed -i.bak "s/android:versionName="\"${VERSIONNAME}\""/android:versionName="\"${BASE_VERSION}.${APPCENTER_BUILD_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak
sed -i.bak "s/android:versionCode="\"1\""/android:versionCode="\"${APPCENTER_BUILD_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak

sed -i.bak "s/package="\"do.gob.ogtic.inspector\""/package="\"${APP_BUNDLE_ID}\""/" ${MANIFEST}
rm -f ${MANIFEST}.bak

# Print out file for reference
cat $MANIFEST
echo "Updated manifest file!"

fi

# Lets work with the splash activity
echo "Updating splash activity..."
sed -i.bak "s/Label="\"Inspector\""/Label="\"${APP_NAME}\""/" ${SPLASH_ACTIVITY}
rm -f ${SPLASH_ACTIVITY}.bak

cat $SPLASH_ACTIVITY
echo "Updated splash activity!"

# Lets work with the appsettings.json
echo "Creating appsettings.json file..."
echo "{
  \"ZammadApiBaseUrl\": \"${ZammadApiBaseUrl}\",
  \"ZammadToken\": \"${ZammadToken}\",

  \"KeycloakBaseUrl\": \"${KeycloakBaseUrl}\",
  \"KeycloakClientId\": \"${KeycloakClientId}\",
  \"KeycloakGrantType\": \"${KeycloakGrantType}\",
  \"KeycloakPassword\": \"${KeycloakPassword}\",
  \"KeycloakUsername\": \"${KeycloakUsername}\",

  \"IncidentsApiBaseUrl\": \"${IncidentsApiBaseUrl}\",
  \"TerritorialDivisionApiBaseUrl\": \"${TerritorialDivisionApiBaseUrl}\",
  \"IAmApiBaseUrl\": \"${IAmApiBaseUrl}\",
  \"IamAuthToken\": \"${IamAuthToken}\",
  \"DigitalGobApiBaseUrl\": \"${DigitalGobApiBaseUrl}\",
  \"XAccessToken\": \"${XAccessToken}\",
}" > ${APPSETTINGS}

cat $APPSETTINGS
echo "Created appsettings.json file!"
