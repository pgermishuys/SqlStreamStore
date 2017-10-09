#!/usr/bin/env bash
test_result=$(grep FAIL build-results.out)
if [[ "$test_result" == "" ]] ; then
    exit 0
else
    exit 1
fi