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
    jobId: string = '';
    constructor(
        private _documentRepo: DocumentationRepo,
        private _ngProgressService: NgProgress,
        private _activedRouter: ActivatedRoute,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

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
    getListAdvanceSettlement(id: any) {
        this._documentRepo.getListAdvanceSettlement(id)
            .pipe(
                takeUntil(this.ngUnsubscribe),
                catchError(this.catchError),
            ).subscribe(
                (res: AdvanceSettlement[]) => {
                    console.log(res);
                    this.adSett = res;
                },
            );
    }
}