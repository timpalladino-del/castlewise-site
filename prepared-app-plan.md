# Household Preparedness App: Research and Product Plan

Working title: none yet (branding deferred). Referred to below as "the app."
Date: November 2, 2026. Research sources are listed at the end.

---

## 1. The idea in one paragraph

A calm, mainstream "stay-ready" app for ordinary households: one place for the food, water, medicine, gear, documents and plans a family keeps for outages, storms, illness and other bad weeks. It borrows the CastleWise model (local-first, no account, one-time price, a hand-built reference library, camera assist) but not its branding. The positioning line the target audience already uses for itself: **"Prep for Tuesday, not doomsday."**

## 2. Who it is for

**Primary:** households that have started buying extra supplies after a storm, outage, or news cycle, and now have stuff in three closets and no idea what is expiring. Research shows this group skews female, millennial and Gen Z, often renters or apartment dwellers, and actively avoids the word "prepper."

- r/TwoXPreppers split from r/preppers in 2022 and is now one of the most active preparedness communities. Its framing: "practical self-care," "insurance," "your doomsday is our Tuesday."
- Finder survey: roughly 40% of Gen Z and 39% of millennials spent money on preparedness in the past year, versus 29% of adults overall.
- SafeHome 2026 survey: only 5% of US homes have a fully stocked kit, 20% have nothing, and cost is the most-cited barrier.
- Government normalization: the EU's 72-hour household self-sufficiency target (2025), the UK "Be Ready" campaign, FEMA's September National Preparedness Month.

**What turns them off:** bunkers, guns, MRE culture, "survive" and "mission" language, doomsday framing, expensive gear, being called paranoid by family. Every prepper-branded app on the App Store today fails this test by name alone.

**Secondary:** experienced preppers who currently run spreadsheets. They will tolerate a mainstream tone if the inventory engine is strong, and they are the loudest reviewers.

## 3. What the research says people struggle with

Ranked by how often it recurred across forums, reviews and press.

1. **Spreadsheets get built, then abandoned.** "Never found time to update it." Vendor estimate: most stop within weeks.
2. **Manual entry kills apps too.** An app that makes you type every item and date "has the same abandonment problem as a spreadsheet." Grocy is praised for scope and abandoned for data entry.
3. **No payoff at the moment of updating.** Logging a used can gives nothing back, so nobody logs it.
4. **Expired and forgotten stock.** A UK prepper opened tins six years past date. The community rule is "only stock what you eat routinely," which requires rotation.
5. **Rotation is "never ending."** People try barcode in/out systems in Excel and give up.
6. **Supplies scattered across locations, duplicates bought.** "If you can't walk to an item from its record, the inventory isn't useful."
7. **Must work with no power and no internet.** "Excel is no good if the power goes out." Several keep a printed binder. Preppr's top review complaint: inventory needs internet.
8. **Beginner overwhelm.** "Totally overwhelmed, frustrated, and underprepared." Advice given: start with low-hanging fruit, think about your actual likely scenarios.
9. **Budget barrier.** Cost is the top reason people have no kit.
10. **Water rotation uncertainty.** Advice ranges from 6 months to 3 years. People put painter's tape with a fill date on the lid.
11. **Medication and first-aid dates.** No tracking method exists beyond "write the purchase date on it."
12. **Forgotten equipment upkeep.** Generator exercise runs, extinguisher gauges, alarm tests, power-station top-ups. Nobody tracks these in a preparedness context.
13. **Unsupportive partner.** Recurring thread topic. A shared, non-weird app is a way to bring a spouse in.
14. **Prepper fatigue and anxiety.** People burn out on worry. Tone matters: the app should reduce anxiety, not feed it.
15. **Go-bag upkeep.** Kids outgrow clothes in the bag, batteries die. The one go-bag app only tracks one bag.

## 4. Competitor gaps

| Gap | Evidence |
|---|---|
| Nobody combines food + water + gear + documents + plans | Pantry apps ignore gear. FEMA and Red Cross apps have checklists but no quantities or expiry. Home-inventory apps ignore consumables. Only Grocy spans food and equipment, and it is self-hosted and "ludicrously complex." |
| No mainstream tone | Every prepper app is named Prepper, Preppr, PrepperPro, Survival Plan. Pantry apps are framed around food waste, not readiness. |
| Local-first is demanded but poorly delivered | PrepperPro wiped inventories on low bandwidth. Preppr needs internet. The offline apps that exist are unpolished Android or web tools. |
| Equipment maintenance reminders do not exist in this category | Generator apps are standalone. Forum users use calendar reminders. |
| Shelf-life knowledge is thin | Barcode lookups give packaged dates only. Nobody ships curated data for water, fuel, batteries, meds, opened versus sealed, or deep storage. |
| Family sharing is the number one reliability complaint | Pantry Check, NoWaste, KitchenPal and Nest Egg all have sync-broke reviews. A working CloudKit share is a differentiator on its own. |
| Abandonment risk | Prep & Pantry last updated 2021. Thrive Life closed 2025. Users want CSV export as insurance. |
| Readiness score / days of supply | Exists only in small Android and web tools. No quality iOS version. |

**Pricing norms:** pantry apps charge $7 to $15 per year or $30 lifetime. One-time micro-prices ($2 to $7) are common and praised. Subscription resentment spikes above $4 per month. Nest Egg's one-time purchase is explicitly praised in reviews. A one-time $9.99 to $14.99 fits the category and avoids its loudest complaint.

## 5. Product principles

1. **Capture in under five seconds.** Barcode, date scan, or one tap on a starter pack. Typing is the fallback, never the default.
2. **Every update pays off immediately.** Marking a can used adds it to the shopping list, updates days-of-supply, and clears an alert. Make the app answer "what should I use this week?" so opening it is useful even when nothing is wrong.
3. **Works in the dark.** Everything on-device. Outage Mode is available with no signal. Printable PDF of the inventory for the binder people.
4. **Calm, not alarming.** Readiness is shown as progress, not a fear score. Copy is practical. No countdowns, no red everywhere.
5. **Honest about dates.** "Best if used by" is quality, not safety, except infant formula. The app explains this instead of screaming "EXPIRED" over a can of beans.
6. **A library, not a guess.** Shelf life, maintenance intervals and checklists come from USDA, CDC, FEMA, NFPA and manufacturer sources, cited in-app. Apple reviewers reject health and safety content with no sources.
7. **No account, no cloud by default, one-time price.** Household sharing via iCloud is the one optional sync, and it is Apple's infrastructure, not ours.
8. **Start small on purpose.** Onboarding asks three questions and produces a short first list, not a 60-item kit.

## 6. Feature plan by phase

### Phase 1: MVP. Solves the user's own problem today.

**Supplies inventory**
- Item record: name, quantity, unit, location, purchase date, expiration or best-by date, "opened" toggle, notes, photo.
- Categories: food, water, medicine and first aid, hygiene, power and light, tools and gear, pet, baby, documents, cash, other.
- Locations are first-class: Pantry, Basement shelf, Car, Go-bag, Office. Multiple containers per location. "Walk to it" test passes.
- Barcode scan via Apple VisionKit, on-device. Product name and image from Open Food Facts (with required attribution), falling back to a local "my products" memory so the second scan of the same item is instant.
- Date scan: point camera at the printed date, OCR on-device, confirm with one tap.
- Shelf-life auto-estimate when no date is printed: USDA FoodKeeper data (public domain, bundled) plus a curated deep-storage table (rice, beans, oats, pasta, honey, salt, sugar, powdered milk, freeze-dried, MREs, canned high-acid versus low-acid).
- Opened versus sealed changes the estimate (oil, peanut butter, flour, powdered milk).
- Quick quantity controls: swipe to use one, long-press to adjust. Bulk "I just did a Costco run" multi-scan mode.

**Expiration alerts and rotation**
- Alert ladder per item class: 60/30/7 days for food, 6 months before for deep storage, custom for meds.
- Weekly "Use this week" digest: a short list of what to cook or swap out, with a one-tap "used it, add to shopping list."
- Rotation queue sorted by date across all categories and locations.
- Snooze, "I checked it, still fine" (extends by a sensible interval for canned goods), and "tossed it."
- Water containers track a fill date, with a 6-month rotation reminder per CDC guidance. Digital version of the tape-on-the-lid trick.

**Equipment with notes and maintenance**
- Equipment record: model, serial, purchase date, fuel type, battery type and count, manual photo, notes, where it is.
- Maintenance schedules from a built-in table, editable: generator exercise run monthly and oil at 100 hours or yearly; fire extinguisher visual check monthly, replace at 12 years; smoke and CO alarms test monthly, battery yearly, replace at 10 years; power station top-up every 3 months; first-aid kit review every 6 months; propane cylinder requalification at 12 years; stored gasoline rotate yearly; sump pump test quarterly; water filter replacement per manufacturer.
- Log entries: "Ran generator 20 min, 11/2/26." The log is the notes history the user asked for.
- Battery tracker: what sizes each device needs versus how many spares you have, with shelf life (alkaline up to 10 years, lithium up to 20).

**Readiness view**
- Household setup: people, pets, dietary notes, location (state or region only, never GPS).
- Days of supply for food, water, and medicine based on household size and Red Cross targets (3 days to evacuate, 2 weeks at home).
- Gap list: what you would need to reach your chosen target, with estimated cost, generated as a shopping list. This is the "payoff" for keeping inventory current.

**Export and trust**
- CSV and PDF export, free, always. Printable binder sheet.
- Full local backup file. Restore from file.

### Phase 2: What makes it a household tool

- **Kits and bags as containers.** Go-bag per person, car kit, office kit, pet kit. Checklist templates from Ready.gov and Red Cross, with "last reviewed" dates and a 6-month review reminder. Kids' kits flag outgrown clothing and seasonal swaps.
- **Household sharing** via CloudKit shared zones. Partner scans a can at the store, it appears on your phone. Build this carefully and test sync failures, because broken sharing is the number one review complaint in the category.
- **Shopping list integration.** Everything consumed or missing flows to one list, grouped by store. Optional Reminders app export.
- **Seasonal readiness checklists.** Hurricane (June to November), wildfire (summer to fall), winter storm, tornado season, heat, earthquake. Shown by the user's region and the calendar. Each is a short "before the season" list mapped to inventory and equipment, so "check generator" is a tap, not a sentence.
- **Documents vault.** FEMA Emergency Financial First Aid Kit checklist: IDs, insurance, deed or lease, medical list, contacts, account list. Photos stored encrypted on-device behind Face ID. Insurance home-inventory walkthrough (room photos plus serials) since 46% of homeowners have none.
- **Medication tracker.** Household meds with quantity and date, 7-day supply target from Red Cross, refill reminders. Neutral on expired meds: shows FDA's position and does not advise use.

### Phase 3: Intelligence and the moment of need

- **Outage Mode.** One big button when the power goes out. Starts the USDA fridge and freezer clock (4 hours fridge, 48 hours full freezer, 24 half-full), shows your flashlight and battery inventory, surfaces the generator CO safety card (20 feet from the house, never in a garage), lists cash on hand and water stored, and works fully offline.
- **Camera check (the FixLens equivalent).** "What is this and is it still good?" for a bulging can, a rusty lid, a mystery MRE date code, a generator fault code, or a battery type. On-device library first, optional Claude-powered check second, clearly labeled, same privacy model as CastleWise.
- **Scenario planner.** "3-day outage, 4 people, winter." Shows what you have, what you lack, and a budgeted shopping list to close the gap at $20 a week. Directly addresses the cost barrier and the overwhelm problem.
- **Know-how library.** Short plain-language cards: water disinfection ratios (8 drops of unscented bleach per gallon, 30 minutes), what to do when the freezer thaws, how to store gasoline, how to test a sump pump. Each card cites its source.
- **Skills and drills.** Twice-yearly family drill reminder tied to daylight saving changes (meet point, where the shutoffs are, who grabs the pets). Light, optional, not gamified into anxiety.
- **Widgets and Siri.** "Hey Siri, we used a can of beans." Lock-screen widget showing items expiring this month.
- **Smart suggestions.** "You have 40 cans of beans and no can opener in the go-bag." Rule-based, from the library.

## 7. More ideas to consider (backlog)

- Receipt scan to batch-add a grocery run.
- "Eat down" recipe suggestions from items expiring soon (local, from a small recipe set).
- Rotation nudge on your grocery day: "Take 2 cans of tomatoes from deep storage to the kitchen, buy 2 to replace."
- Freezer and fridge inventory with power-outage safety logic built in.
- Vehicle kit with seasonal swap list and "keep tank half full in winter" reminder.
- Neighbor and family contact card that works offline, printable.
- Pet profile: food days, meds, vet records, current photo for lost-pet flyers.
- Budget tracker: what you spent on readiness this year, and what it would cost to replace.
- Insurance claim helper: export photos and serials in the format adjusters ask for.
- "Borrowed and lent" for shared generators, chainsaws, coolers.
- Expiration date for cash? No. But a reminder to keep small bills, and a count.
- Community templates: share a kit template as a file, no server needed.
- Apple Watch: tick off a kit check while standing in the garage.
- Printable QR labels for totes that open the tote's inventory.
- Fuel log: gallons stored, stabilizer added date, rotate-by date.
- Chest freezer map (grid like the CastleWise panel map): what is in which basket.

## 8. Expiration and alert design, in detail

Since this is the problem the user is hitting right now, the rules the app should follow:

**Date source priority**
1. Printed date on the package, scanned or typed.
2. FoodKeeper estimate from purchase date (pantry, opened or unopened).
3. Deep-storage table if the item is flagged as long-term storage (cool, dry, oxygen-free assumption shown).
4. Category default, clearly marked as a guess.

**Language**
- Food: "Best by" and "Use first," not "Expired," unless the item is infant formula, medicine, or a user-set hard date.
- Canned goods past date and intact: "Past best-by date. Safe if the can is not rusted, dented or swollen." Offer "Checked, still fine" and "Toss."
- Medicine: show the date and FDA guidance, never a "still fine" button.

**Alert cadence**
- One weekly digest by default. Per-item pings only for medicine, water rotation and equipment tasks. People already have alert fatigue.
- Reminders scheduled by iOS locally, no server, so they fire during an outage too.

**Capture flows ranked by speed**
1. Rescan a known barcode: 1 tap, quantity +1.
2. Starter pack ("Costco emergency bucket," "Red Cross 3-day kit"): 1 tap adds all lines.
3. New barcode: scan, confirm name, scan date, done.
4. No barcode (bulk rice, home-canned): type name, pick category, FoodKeeper fills the rest.

## 9. Content library to build (the moat)

All of this ships inside the app, offline, with citations.

| Content | Source | License status |
|---|---|---|
| Everyday shelf life, ~650 foods | USDA FoodKeeper JSON (data.gov) | CC0, bundle freely |
| Deep-storage shelf life | LDS Home Storage, BYU 30-year studies, university extensions | Paraphrase and cite, do not copy tables |
| Power-outage food safety | USDA FSIS | Public domain |
| Water storage and disinfection | CDC | Public domain |
| Kit checklists | Ready.gov, FEMA | Public domain. Re-verify wording; Ready.gov shows an "under revision" banner. |
| Red Cross kit targets | Red Cross | Paraphrase and cite |
| Document checklist | FEMA EFFAK (P-2063) | Public domain |
| Alarm and extinguisher intervals | NFPA via state fire marshal pages, USFA | Cite fire-marshal pages, NFPA standards are paywalled |
| Generator intervals and CO safety | CPSC, Generac and Honda manuals | Cite manufacturer |
| Battery, power station, filter life | Manufacturer pages | Cite manufacturer |
| Seasonal hazard calendars | NWS, NHC, CAL FIRE Ready Set Go | Public domain, cite |
| Medication expiration context | FDA SLEP studies (Lyon 2006, Cantrell 2012) | Cite, present neutrally |

## 10. Technical plan

- **Platform:** iOS 17+, SwiftUI, SwiftData (or Core Data if CloudKit sharing is needed in Phase 2, since shared zones are easier there). iPhone first, iPad layout later.
- **Scanning:** VisionKit DataScannerViewController for barcode and text in one view. Custom date parser for "BEST BY 03/27," "EXP 2027-03," "BB 27MAR," Julian codes where known.
- **Product lookup:** Open Food Facts API with a proper User-Agent. ODbL: attribution on screens where their data appears and in the store listing. Do not mix OFF data into a redistributed database. Cache per-device only. Fallback: user-named products stored locally and reused.
- **Shelf life:** FoodKeeper JSON bundled and indexed at build time. Deep-storage table hand-written as JSON with source fields.
- **Notifications:** UNUserNotificationCenter local scheduling. Weekly digest plus per-item exceptions.
- **Storage and privacy:** on-device only. Documents vault encrypted with a key in the Secure Enclave, Face ID gated. No analytics SDK; anonymous counts only, opt-out, as in CastleWise.
- **Sharing (Phase 2):** CloudKit shared record zones. Design for conflict resolution and offline queues from day one. Test the "partner's phone is in airplane mode for a week" case.
- **Export:** CSV, PDF (binder sheet), JSON backup. All free.
- **Camera check (Phase 3):** reuse the CastleWise Expert Check server pattern (anonymous per-install ID, rate limit, nothing stored).
- **App Store:** cite sources in-app for health and safety content (Guideline 1.4.1). Add "not a substitute for 911 or medical advice" and "check with a doctor" where medicine appears. Avoid a checklist-only build (Guideline 4.2.2 rejects content aggregators), which the inventory engine satisfies.

## 11. Pricing

- **Free, always:** inventory up to 50 items, barcode and date scanning, shelf-life library, Outage Mode, export, know-how cards, one kit.
- **One-time Pro, $12.99 (test $9.99 to $14.99):** unlimited items, equipment maintenance schedules, seasonal checklists, documents vault, household sharing, unlimited kits, widgets.
- No subscription. Say so on the store page; it is the category's loudest complaint and CastleWise already proved the message.
- Family Sharing enabled on the purchase, so a spouse does not pay twice.

## 12. Voice (not branding)

Branding, name and visual identity are deferred. Voice should still be decided early because it shapes copy in every screen.

- Say: ready, stocked, supplies, use first, bad week, outage, storm, "keep extra of what you already buy."
- Avoid: prepper, survival, SHTF, bug out, tactical, apocalypse, doomsday, mission, loadout, threat.
- Tone: a competent friend who has been through a few outages. Short sentences. No fear.
- Readiness is shown as "You're covered for 9 days of water" rather than a percentage score.

## 13. Roadmap

| Weeks | Milestone |
|---|---|
| 1 to 2 | Data model, FoodKeeper import, deep-storage table v1, maintenance table v1 |
| 3 to 5 | Inventory, locations, barcode and date scanning, quick quantity controls |
| 6 to 7 | Alerts, weekly digest, rotation queue, water fill dates |
| 8 to 9 | Equipment records, maintenance schedules and logs, battery tracker |
| 10 | Readiness view, household setup, gap shopping list |
| 11 | Export, backup, onboarding with three questions and starter packs |
| 12 | TestFlight beta, 30 to 50 testers |
| Phase 2 | Kits, sharing, seasonal checklists, documents, meds (roughly 10 more weeks) |

**Beta recruitment:** r/TwoXPreppers and r/preppers both restrict self-promotion, so ask moderators first or recruit via the CastleWise beta list, theprepared.com forum, and homesteading and frugal-living groups. Pitch it as "a pantry and supplies tracker that works offline," not as a prepper app.

**Success signals for the MVP:** testers still logging items after 4 weeks (the abandonment problem), weekly digest open rate, number of items with a scanned date versus typed, and whether anyone exports a PDF.

## 14. Risks and open questions

- **Date OCR accuracy.** Printed dates vary wildly. Ship it as "scan then confirm," never auto-commit. Measure confirmation edits in beta.
- **Open Food Facts coverage** for US store brands is uneven. Local product memory and starter packs cover the gap.
- **Sharing complexity.** Phase 2, not MVP. Do not promise it until it is reliable.
- **Scope creep.** The backlog in section 7 is long on purpose. The MVP is inventory, alerts, equipment, readiness, export. Nothing else.
- **Tone drift.** Experienced preppers will ask for ammo categories and threat levels. A custom category covers it without changing the voice.
- **Content maintenance.** Government pages change (Ready.gov is mid-revision). Keep source URLs and a "last checked" date on every library entry.
- **Open question:** does this live inside CastleWise (it already has a free "Emergency mode") or as a separate app? Research favors a separate app with its own audience and store listing, sharing code and the Expert Check backend. A homeowner-tool brand will not reach renters and apartment dwellers, who are a large share of the target audience.

## 15. Research caveats

- Reddit itself was unreachable from this environment. Reddit signal came from Slate, MetaFilter and Yahoo coverage of r/TwoXPreppers and r/UKPreppers threads, plus theprepared.com and survivalistboards.com forums, which were reachable via search snippets only.
- App Store pages were also blocked, so ratings and review quotes come from search snippets and aggregators and should be treated as approximate.
- Government sites (ready.gov, cdc.gov, fsis.usda.gov) were blocked, so those facts were confirmed against archived copies and mirrors. Re-verify exact wording before shipping library content.

## 16. Sources

**Audience and pain points**
- https://slate.com/technology/2022/03/reddit-preppers-sub-women-splitting-off-twoxpreppers.html
- https://www.metafilter.com/194896/Your-doomsday-is-our-Tuesday
- https://theassessment.substack.com/p/prep-for-tuesday-not-doomsday
- https://www.yahoo.com/lifestyle/articles/uk-prepper-opens-undamaged-food-090100448.html
- https://www.yahoo.com/lifestyle/articles/stock-straight-away-uk-preppers-232500874.html
- https://uk.news.yahoo.com/uk-mum-ran-money-emergency-113300016.html
- https://www.yahoo.com/lifestyle/articles/uk-preppers-want-rebrand-less-133700937.html
- https://www.yahoo.com/lifestyle/articles/meet-women-redefining-prepping-practical-110000867.html
- https://theprepared.com/forum/thread/food-prep-inventory-tracking/
- https://theprepared.com/forum/thread/prepping-organization-diary-journals-todo-lists-etc/
- https://theprepared.com/forum/thread/water-storage/
- https://theprepared.com/forum/thread/first-aid-expiration-dates/
- https://theprepared.com/forum/thread/newbie-prepper/
- https://theprepared.com/forum/thread/dealing-with-a-spouse-significant-other-who-thinks-prepping-is-nuts/
- https://www.survivalistboards.com/threads/how-do-you-inventory-your-preps.271163/
- https://www.survivalistboards.com/threads/prep-organization-the-best-prep-inventory-system.978290/
- https://www.survivalistboards.com/threads/pantry-inventory-app-software.1004774/
- https://tryinstock.com/blog/pantry-inventory-spreadsheet-vs-app/
- https://www.wellstockd.com/blog/shared-pantry-app
- https://www.xda-developers.com/grocy-seemed-like-a-great-self-hosted-tool-until-i-used-it-for-a-week/
- https://modernsurvivalblog.com/preps/time-to-run-your-generator/
- https://www.finder.com/doomsday-prepper-statistics
- https://www.safehome.org/research/home-emergency-preparedness-study/
- https://www.globenewswire.com/news-release/2026/09/15/3362270/0/en/only-13-of-americans-have-an-emergency-plan-first-onsite-survey-finds.html
- https://www.cnn.com/2025/06/17/climate/left-wing-liberal-preppers
- https://theweek.com/defence/be-ready-prepping-goes-mainstream
- https://time.com/7000176/costco-sells-emergency-food-bucket/

**Competitors**
- https://apps.apple.com/us/app/pantry-check-grocery-list/id966702368
- https://apps.apple.com/us/app/nowaste-food-inventory-list/id926211004
- https://apps.apple.com/us/app/fridgely/id988016972
- https://apps.apple.com/us/app/grocy-mobile/id1567803209
- https://apps.apple.com/us/app/prepperpro/id6743128293
- https://apps.apple.com/app/id6741197301
- https://apps.apple.com/us/app/preppr-learn-stock-survive/id6670330104
- https://apps.apple.com/us/app/inventory-wolf/id1514682383
- https://apps.apple.com/app/id431188993
- https://apps.apple.com/us/app/go-bag-emergency-disaster-kit/id1619012440
- https://beagleprep.com/blog/best-prepper-inventory-apps/
- https://www.askaprepper.com/ppantry-app-review-the-best-free-tool-for-tracking-your-emergency-supplies/
- https://thesurvivalmom.com/best-food-storage-app/
- https://pare.recipes/blog/best-pantry-inventory-apps
- https://www.capterra.com/p/169199/Sortly-Pro/reviews/
- https://appfollow.io/ios/fema/474807486?country=us
- https://appgoblin.info/apps/954783878
- https://glenbrook.com/payments_news/us-subscription-fatigue-is-real-with-consumers-managing-an-average-of-5-accounts/

**Official guidance and data**
- https://catalog.data.gov/dataset/fsis-foodkeeper-data
- https://www.foodsafety.gov/keep-food-safe/foodkeeper-app
- https://www.ready.gov/kit
- https://www.redcross.org/get-help/how-to-prepare-for-emergencies/survival-kit-supplies.html
- https://www.cdc.gov/water-emergency/about/how-to-create-and-store-an-emergency-water-supply.html
- https://www.fsis.usda.gov/food-safety/safe-food-handling-and-preparation/food-safety-basics/food-safety-during-power-outage
- https://www.fda.gov/food/buy-store-serve-safe-food/confused-date-labels-packaged-foods
- https://www.churchofjesuschrist.org/study/manual/food-and-water-storage/long-term-food-and-water-storage
- https://pubmed.ncbi.nlm.nih.gov/16721796/
- https://www.cpsc.gov/Newsroom/News-Releases/2023/New-CPSC-Report-Shows-Upward-Trend-in-Carbon-Monoxide-CO-Fatalities
- https://www.ready.gov/sites/default/files/2020-03/ready_emergency-financial-first-aid-toolkit.pdf
- https://www.ready.gov/pets
- https://www.nfpa.org/education-and-research/home-fire-safety/smoke-alarms
- https://dps.mn.gov/divisions/sfm/services/inspections/hotels-and-motels/hotel-fire-extinguishers
- https://support.generac.com/articles/Knowledge/How-Often-Should-I-Perform-Routine-Maintenance-on-My-Portable-Generator
- https://www.jackery.com/blogs/buying-advice/how-long-does-a-jackery-hold-a-charge
- https://www.redcross.org/take-a-class/resources/articles/do-first-aid-kits-expire
- https://www.law.cornell.edu/cfr/text/49/180.209
- https://www.osti.gov/servlets/purl/1860579
- https://readyforwildfire.org/wp-content/uploads/2025/04/1-Hour-Wildfire-Ready-Checklist.pdf
- https://www.weather.gov/news/240103-spring-safety-campaign
- https://www.insurancejournal.com/news/national/2024/06/20/780393.htm
- https://developer.apple.com/app-store/review/guidelines/

**Technical**
- https://openfoodfacts.github.io/documentation/docs/Product-Opener/api/tutorials/license-be-on-the-legal-side/
- https://support.openfoodfacts.org/help/en-gb/12-api-data-reuse/94-are-there-conditions-to-use-the-api
- https://nonstrict.eu/wwdcindex/wwdc2022/10025/

---

## 17. Name candidates (added November 2, 2026)

Brief: calm, mainstream, household. Says "get through a bad week," never "survive the end." Should be easy to say
and spell, work as an App Store title with a subtitle, and sit comfortably next to a Costco receipt rather than a
bunker. Web search was used to check for same-name apps in this category; apps.apple.com and USPTO were not
reachable from this environment, so every finalist still needs a store search and a trademark search.

| Name | Why it fits | Tagline sketch | Conflicts found |
|---|---|---|---|
| **Tide Over** (recommended) | "Enough to tide you over" is the exact promise: get through, not survive. Gentle idiom everyone knows. Zero tactical energy. | Enough to tide you over. | None in apps or trademarks. "Tide" detergent and Tide banking are unrelated classes; still check. |
| **Lantern** | The object you reach for when the lights go out. Warm, protective, domestic. Reads well as an icon. | Ready for the dark. | No preparedness or pantry app. A defunct 2014 DoE Android app "Lantern Live." Common word; expect other Lanterns in unrelated categories. |
| **Just in Case** | The phrase non-preppers already use to explain the extra case of water. States the whole philosophy. | Keep a little extra, just in case. | No preparedness app found. Generic phrase, weak trademark, poorer search discoverability. |
| **Backstock** | Retail term for the extra inventory in the back room. Precise: this audience has a backstock, not a bunker. Distinctive and ownable. | Know what's in the back. | None found. Slightly insider; less warm than the others. |
| **Rainy Day** | "Saving for a rainy day." Planning without fear, fully mainstream. | For a rainy day. | Several unrelated apps named Rainy Day (sleep sounds, a finance game). Crowded word, discoverability cost. |
| **Tuesday** (wildcard) | From the community's own line, "prep for Tuesday, not doomsday." Opaque alone, memorable with a subtitle. | Ready for Tuesday. | None found in category. Generic word, weak trademark. |
| **StockWise** (in-family option) | Sibling to CastleWise if you ever want a family of "-Wise" apps. | The stocked-up house. | "Stock" leans financial. Only worth it if the family brand matters. |

Rejected after checking: Larder (two pantry apps), Squirrel (direct competitor "Squirrel: Home Inventory"),
Kept (home inventory app), Shelf Life (four apps), Provisions ("PROvisions: Expiry Tracker" exists),
Stocked and Stockpile (existing competitors), anything with Prep, Ready, Survival or Shield in it.
