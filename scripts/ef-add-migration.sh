#!/bin/bash

clear

echo "Název migrace:"
read ident

dotnet ef migrations add $ident --project ../src/Application.Lib --startup-project ../src/Application.Web

read -n 1 -s