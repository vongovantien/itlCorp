import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-commercial-incoterm',
    templateUrl: './commercial-incoterm.component.html',
})
export class CommercialIncotermComponent extends AppList implements OnInit {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
