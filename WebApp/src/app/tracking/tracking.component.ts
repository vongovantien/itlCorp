import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-tracking',
    templateUrl: './tracking.component.html',
    styleUrls: ['./tracking.component.scss']
})
export class TrackingComponent implements OnInit {
    isHome: Boolean = false;
    constructor() { }

    ngOnInit() {
    }
    getValueSearch(event) {
        this.isHome = event
    }

}
