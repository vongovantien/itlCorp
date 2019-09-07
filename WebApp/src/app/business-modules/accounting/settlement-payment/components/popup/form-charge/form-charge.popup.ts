import { Component } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { takeUntil, debounceTime, switchMap, skip, distinctUntilChanged } from 'rxjs/operators';
import { BehaviorSubject, Observable } from 'rxjs';
import { Charge } from 'src/app/shared/models';

const autocomplete = (time, selector) => (source$) =>
    source$.pipe(
        debounceTime(time),
        distinctUntilChanged(),
        switchMap((...args: any[]) =>
            selector(...args)
                .pipe(
                    takeUntil(
                        source$
                            .pipe(
                                skip(1)
                            )
                    )
                )
        )
    );

@Component({
    selector: 'form-charge-popup',
    templateUrl: './form-charge.popup.html',
    styleUrls: ['./form-charge.popup.scss']
})

export class SettlementFormChargePopupComponent extends PopupBase {

    isShow: boolean = false;
    term$ = new BehaviorSubject<string>('');
    $charges: Observable<any>;
    selectedCharge: any = null;
    constructor(
        private _systemRepo: SystemRepo
    ) {
        super();
    }

    ngOnInit() {
        this.$charges = this.term$.pipe(
            distinctUntilChanged(),
            this.autocomplete(1000, (term => this._systemRepo.getListCharge()))
        );
    }

    onSearchAutoComplete(keyword: string) {
        this.term$.next(keyword);
    }

    autocomplete = (time: number, callBack: Function) => (source$: Observable<any>) =>
        source$.pipe(
            debounceTime(time),
            distinctUntilChanged(),
            switchMap((...args: any[]) =>
                callBack(...args).pipe(takeUntil(source$.pipe(skip(1))))
            )
        )

    selectCharge(charge: any) {
        this.selectedCharge = charge;
    }

}
