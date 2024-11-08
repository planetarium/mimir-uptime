# Mimir Uptime

This repository monitors the uptime and data accuracy of the [Mimir service](https://github.com/planetarium/mimir) for the NineChronicles blockchain.

## Overview

Using GitHub Actions, this repository runs tests every 10 minutes to verify that the data provided by Mimir aligns with the actual data on the NineChronicles chain. The comparison includes a specific block index, ensuring the data is accurate to that precise point in time. For example, if the block index is 10, the test compares Mimir’s data against the chain data at block index 10, avoiding timing inconsistencies.

## How to Use

If you're actively running Mimir and want to monitor its data integrity, simply fork this repository and set it up in your environment. The tests will automatically run to check for data accuracy and uptime.

## Notifications and Sync Check

- **Slack Notifications**: Slack notifications are configured to alert you in case of a test failure.

## Current Collections Tested
The tests cover the following collections:

- action_point
- agent
- avatar
- daily_reward
- ~~inventory~~ (disabled)
- stake
- world_information
