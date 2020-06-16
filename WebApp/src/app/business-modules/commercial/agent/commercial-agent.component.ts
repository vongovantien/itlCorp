import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-commercial-agent',
    templateUrl: './commercial-agent.component.html',
})
export class CommercialAgentComponent extends AppList implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}

