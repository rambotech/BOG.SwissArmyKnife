#!/bin/bash
set -ev
set echo off

pwd

echo NUGET_API_KEY
echo NUGET_SOURCE == ${NUGET_SOURCE} 
echo TRAVIS_PULL_REQUEST == ${TRAVIS_PULL_REQUEST}

if [ "${BUILD_CONFIG}" = "release" ]; then
		BUILD_DIR="release"
else
		BUILD_DIR=$BUILD_CONFIG
fi

dotnet build -c $BUILD_CONFIG ./src/BOG.SwissArmyKnife.sln

if [ $? -eq 0 ]
then
		if [ "$BUILD_CONFIG" = "not_happening" ]; 
		then
				dotnet test ./src/BOG.SwissArmyKnife.Test/BOG.SwissArmyKnife.Test.csproj -v normal --no-build
		else
				echo Skip testing on non-debug build
		fi
fi

if [ $? -eq 0 ]
then
		if [ "${TRAVIS_PULL_REQUEST}" = "false" ] && [ "${TRAVIS_BRANCH}" = "master" ]; 
		then
				dotnet nuget push ./src/BOG.SwissArmyKnife/BOG.SwissArmyKnife/bin/$BUILD_DIR/BOG.SwissArmyKnife.*.nupkg --api-key $NUGET_API_KEY --source $NUGET_SOURCE --skip-duplicate
		else
				echo Skip nuget for non-master build
		fi
fi
