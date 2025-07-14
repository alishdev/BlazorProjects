# Test script for Librarian Crawler System

Write-Host "Testing Librarian Crawler System..." -ForegroundColor Green

# Test 1: Build all projects
Write-Host "`n1. Building all projects..." -ForegroundColor Yellow
dotnet build Librarian.sln
if ($LASTEXITCODE -eq 0) {
    Write-Host "   Build successful" -ForegroundColor Green
} else {
    Write-Host "   Build failed" -ForegroundColor Red
    exit 1
}

# Test 2: Test FileCrawler directly
Write-Host "`n2. Testing FileCrawler directly..." -ForegroundColor Yellow
$testDir = "C:\Temp\test-crawler"
if (!(Test-Path $testDir)) {
    New-Item -ItemType Directory -Path $testDir -Force | Out-Null
}

# Create some test files
"Test content 1" | Out-File -FilePath "$testDir\file1.txt"
"Test content 2" | Out-File -FilePath "$testDir\file2.txt"
New-Item -ItemType Directory -Path "$testDir\subdir" -Force | Out-Null
"Test content 3" | Out-File -FilePath "$testDir\subdir\file3.txt"

# Test the crawler
Add-Type -Path ".\Librarian.Core\bin\Debug\net8.0\Librarian.Core.dll"
Add-Type -Path ".\FileCrawler\bin\Debug\net8.0\FileCrawler.dll"

$crawler = New-Object FileCrawler.FileCrawler
$crawler.Run($testDir)

Write-Host "   FileCrawler test completed" -ForegroundColor Green

# Test 3: Check if Service can be run (compilation test)
Write-Host "`n3. Testing Service compilation..." -ForegroundColor Yellow
$serviceExe = ".\Librarian.Service\bin\Debug\net8.0\Librarian.Service.exe"
if (Test-Path $serviceExe) {
    Write-Host "   Service executable created" -ForegroundColor Green
} else {
    Write-Host "   Service executable not found" -ForegroundColor Red
}

# Test 4: Check if Scheduler can be run (compilation test)
Write-Host "`n4. Testing Scheduler compilation..." -ForegroundColor Yellow
$schedulerDll = ".\Librarian.Scheduler\bin\Debug\net8.0\Librarian.Scheduler.dll"
if (Test-Path $schedulerDll) {
    Write-Host "   Scheduler application created" -ForegroundColor Green
} else {
    Write-Host "   Scheduler application not found" -ForegroundColor Red
}

# Clean up test files
Remove-Item -Path $testDir -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "`nTesting completed!" -ForegroundColor Green
Write-Host "`nTo run the system:" -ForegroundColor Cyan
Write-Host "1. Install the Windows Service: sc create `"Librarian Crawler`" binPath=`"$(Get-Location)\Librarian.Service\bin\Debug\net8.0\Librarian.Service.exe`""
Write-Host "2. Start the Scheduler UI: dotnet run --project Librarian.Scheduler"
Write-Host "3. Access the scheduler at: http://localhost:5000"