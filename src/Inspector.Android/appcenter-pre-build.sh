#!/usr/bin/env bash

MAINACTIVITY="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/SplashActivity.cs"
MANIFEST_CITIZEN="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifest.xml"
MANIFEST_AGENT="$BUILD_REPOSITORY_LOCALPATH/src/Inspector.Android/Properties/AndroidManifestAgent.xml"
APPSETTINGS="$BUILD_REPOSITORY_LOCALPATH/src/Inspector/appsettings.json"

if [ "$APPCENTER_XAMARIN_CONFIGURATION" = "Release" ]
then
  MANIFEST=$MANIFEST_CITIZEN
else
  MANIFEST=$MANIFEST_AGENT
fi

############################# Chenges on manifest files
echo "##[section]Starting: Updating $MANIFEST file..."
echo "=============================================================================="
sed -i '' "s/versionName=\"[0-9.]*\"/versionName="\"${VERSION_NAME}.${APPCENTER_BUILD_ID}\""/" ${MANIFEST}
sed -i '' "s/versionCode=\"1\"/versionCode=\"${APPCENTER_BUILD_ID}\"/" ${MANIFEST}
sed -i '' "s/package=\"[-a-zA-Z0-9_ .]*\"/package=\"${APP_BUNDLE_ID}\"/" ${MANIFEST}

cat $MANIFEST
echo "=============================================================================="
echo "##[section]Finishing: Updated manifest file!"
echo "------------------------------------------------------------------------------"

############################# Changes on Main Activity
echo "##[section]Starting: Updating app name to $APP_DISPLAY_NAME in Main Activity"
echo "=============================================================================="
sed -i '' "s/Label=\"[-a-zA-Z0-9_ ]*\"/Label=\"${APP_DISPLAY_NAME}\"/" ${MAINACTIVITY}

cat $MAINACTIVITY
echo "=============================================================================="
echo "##[section]Finishing: Updated Main Activity!"
echo "------------------------------------------------------------------------------"

############################# Changes on appsettings.json
echo "##[section]Starting: Creating appsettings.json file..."
echo "=============================================================================="
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
echo "=============================================================================="
echo "##[section]Finishing: Created appsettings.json file!"
echo "------------------------------------------------------------------------------"
