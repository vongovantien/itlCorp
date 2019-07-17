import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'soa-search-box',
    templateUrl: './search-box-soa.component.html',
    styleUrls: ['./search-box-soa.component.scss']
})
export class StatementOfAccountSearchComponent extends AppPage {
    constructor() {
        super();
    }

    ngOnInit(): void { }
}
