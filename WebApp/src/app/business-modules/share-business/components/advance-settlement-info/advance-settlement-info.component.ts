import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, takeUntil } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { AdvanceSettlement } from 'src/app/shared/models/operation/advanceSettlement';
import { DocumentationRepo } from 'src/app/shared/repositories/documentation.repo';
import { SortService } from 'src/app/shared/services/sort.service';

@Component({
    selector: 'advance-settlement-info',
    templateUrl: './advance-settlement-info.component.html',
    styleUrls: ['./advance-settlement-info.component.scss']
})

export class ShareBusinessAdvanceSettlementInforComponent extends AppList {
    adSett: AdvanceSettlement[] = [];
    // selectAdSett: AdvanceSettlement = null;
    jobId: string = '';
    // headers: CommonInterface.IHeaderTable[];
    HEROES = [
        { id: 1, name: 'Superman' },
        { id: 2, name: 'Batman' },
        { id: 5, name: 'BatGirl' },
        { id: 3, name: 'Robin' },
        { id: 4, name: 'Flash' }
    ];

    constructor(
        private _sortService: SortService,
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
        private _toastService: ToastrService,
        private _activedRouter: ActivatedRoute,
    ) {
        super();
        // this.headers = [
        //     { title: 'No', field: 'orderNumberProcessed', width: 10 },
        //     { title: 'Requester', field: 'requester', sortable: true },
        //     { title: 'Advance No', field: 'advanceNo', sortable: true },
        //     { title: 'Advance Amount', field: 'advanceAmount', sortable: true },
        //     { title: 'Status Approval', field: 'statusApproval', sortable: true },
        //     { title: 'Settlement Amount', field: 'settlementAmount', sortable: true },
        //     { title: 'Settlement Status Approval', field: 'settlementStatusAproval', sortable: true },
        //     { title: 'Balance', field: 'balance', sortable: true },
        //     { title: 'Settlement No', field: 'settlementNo', sortable: true },
        //     { title: 'Advance Date', field: 'advanceDate', sortable: true },
        //     { title: 'Settlement Date', field: 'settlementDate', sortable: true },
        // ];
        this._progressRef = this._ngProgressService.ref();
        // this.requestList = this.getListAdvanceSettlement;
        // this.requestSort = this.sortAdSettList;
    }
    ngOnInit() {
        this._activedRouter.params.subscribe((param: any) => {
            console.log(param);
            if (param.id) {
                this.jobId = param.id;
                this.getListAdvanceSettlement(this.jobId);
            }
            console.log(this.jobId);

        });

    }
    // ngAfterContentInit() {
    //     this.getListAdvanceSettlement(this.jobId);
    // }
    getListAdvanceSettlement(id: any) {
        this._documentRepo.getListAdvanceSettlement(id)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError),
            ).subscribe(
                (res: AdvanceSettlement[]) => {
                    console.log(res);
                    if (res instanceof Error) {

                    } else {
                        this.adSett = this._sortService.sort(res.map((item: any) => new AdvanceSettlement(item)), 'orderNumberProcessed', true);
                    }
                },
            );
    }

    // sortAdSettList(sortField: string, order: boolean) {
    //     this.adSett = this._sortService.sort(this.adSett, sortField, order);
    // }

}