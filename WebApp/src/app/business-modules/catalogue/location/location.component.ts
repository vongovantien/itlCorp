import { Component, OnInit, AfterViewInit } from "@angular/core";

import { AppList } from "src/app/app.list";

type LOCATION_TAB = 'country' | 'city' | 'district' | 'town';

enum LocationTab {
    COUNTRY = 'country',
    CITY = 'city',
    DISTRICT = 'district',
    TOWN = 'town'
}

@Component({
    selector: 'app-location',
    templateUrl: './location.component.html'
})


export class LocationComponent extends AppList implements OnInit, AfterViewInit {

    selectedTab: LOCATION_TAB = LocationTab.COUNTRY; // Default tab.

    constructor(
    ) {
        super();
    }

    ngOnInit() {

    }

    onSelectTabLocation(tabname: LOCATION_TAB) {
        this.selectedTab = tabname;
    }
}
