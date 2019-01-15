# Grand Channel Inc. Enterprise Resource Planning system

### Version & Update Date
beta v0.3.0 01/14/2019

Beta v0.1.1 09/05/2018

### Introduction
Currently, this system is a solution designed to automatize clothes refine business management in the warehouse of Grand Channel. Due to the particularity of the business, there is currently no software on the market that fits the business perfectly as far as we known. Therefore, the system is specially designed by software developers within the company for suiting the clothes refine business.

01/14/2019 updates: Added FBA system.

### About the release version
After 2 weeks test(08/20~08/31), the first released version Beta v0.1.1 is released on September 5th, 2018

After 3 months further test(09/01/2018~12/31/2018), the second released version Beta v0.3.0 released on 01/02/2019

### Features
This system now has the ability to:
- Create new Pre-receive Order
- Handle and extract uploaded original packing list from Free Country
- Handle receiving process(Inbound)
- Generate receiving a report for per container
- Add a comment for each received item
- Allocate warehouse locations(Put away)
- Reallocate warehouse location(relocate)
- Manage inventory addition(Inbound) and deduction(Outbound)
- Create ship order
- Handle uploaded pull sheet
- Handle picking process
- Generate downloadable PDF version of picking lists
- Diagnose exception in the picking process 
- Generate picking summary
- Put back any outbound cartons and units to inventory

This system can also handle exceptions as follow:
- Free Country sometimes may send packing lists that have extra columns
- Free Country sometimes may send packing lists that do not follow its template
- Free Country sometimes may send packing lists that the summary does not match details
- Free Country sometimes may send extra cartons under some new POs that does not include in the original packing list
- Free Country sometimes may send extra items(SKU) under existed PO
- Free Country sometimes may send existed items(SKU) but under different PO or color
- Free Country sometimes may send some solid pack cartons mixed multiple sizes and units
- Free Country sometimes may send short and extra items related the packing list
- Sometimes multiple different SKU cartons may be requested to allocate to one location
- Sometimes one SKU cartons may be requested to divided and store into multiple locations
- Sometimes PSI information may be not correct
- Sometimes Pull Sheet information may be not correct
- Sometimes shortage may appear in the picking process
- Sometimes concealed overage may appear after picking
- Free Country/customer sometimes may request to send bulk cartons sample
- Free Country/customer sometimes may request to send bulk pieces sample
- Free Country/customer sometimes may request to sent bulk pieces sample from In&Out cartons

### Limited
Currently, the system is unable to:
- Generate invoices
- Handle other customer's business

### License
This is not an open source project. Any usage and Fork are forbidden.

Copyright (c) 2018, Grand Channel Inc. All rights reserved.
