import { Component, ChangeDetectionStrategy } from '@angular/core';
import { AppPage } from 'src/app/app.base';

@Component({
    selector: 'profit-summary',
    templateUrl: './profit-summary.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})

export class ProfitSummaryComponent extends AppPage {
    constructor() {
        super();
    }

    ngOnInit() { }
}