import { Component } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { combineLatest } from 'rxjs';
import { map } from 'rxjs/operators';
import { SeaFCLImportCreateJobComponent } from '../create-job/create-job-fcl-import.component';
import { DocumentationRepo } from 'src/app/shared/repositories';

type TAB = 'SHIPMENT' | 'CDNOTE';
@Component({
    selector: 'app-detail-job-fcl-import',
    templateUrl: './detail-job-fcl-import.component.html',
    styleUrls: ['./../create-job/create-job-fcl-import.component.scss']
})
export class SeaFCLImportDetailJobComponent extends SeaFCLImportCreateJobComponent {

    jobId: string;
    selectedTab: TAB = 'SHIPMENT';

    constructor(
        protected _router: Router,
        protected _documenRepo: DocumentationRepo,
        private _activedRoute: ActivatedRoute
    ) {
        super(_router, _documenRepo);
    }

    ngOnInit(): void {
        combineLatest([
            this._activedRoute.params,
            this._activedRoute.queryParams
        ]).pipe(
            map(([params, qParams]) => ({ ...params, ...qParams })),
        )
            .subscribe((param: any) => {
                this.selectedTab = !!param.tab ? param.tab.toUpperCase() : 'SHIPMENT';
                this.jobId = !!param.id ? param.id : '';
            });
    }

    onSelectTab(tabName: string) {
        switch (tabName) {
            case 'hbl':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}/hbl`]);
                break;
            case 'shipment':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'SHIPMENT' } });
                break;
            case 'cdNote':
                this._router.navigate([`home/documentation/sea-fcl-import/${this.jobId}`], { queryParams: { tab: 'CDNOTE' } });
                break;
        }
    }
}
