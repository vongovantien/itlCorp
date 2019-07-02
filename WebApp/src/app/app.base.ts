import {
    OnInit,
    OnDestroy,
    OnChanges,
    DoCheck,
    AfterContentChecked,
    AfterContentInit,
    AfterViewChecked,
    AfterViewInit
} from "@angular/core";

export abstract class AppPage
    implements
        OnInit,
        OnDestroy,
        OnChanges,
        DoCheck,
        AfterContentChecked,
        AfterContentInit,
        AfterViewChecked,
        AfterViewInit {
    constructor() {}

    ngOnInit(): void {}

    ngOnDestroy(): void {}

    ngDoCheck(): void {}

    ngOnChanges(changes: any): void {}

    ngAfterContentInit(): void {}

    ngAfterContentChecked(): void {}

    ngAfterViewInit(): void {}

    ngAfterViewChecked(): void {}

    trackByFn(index: number, item: any) {
        return !!item.id ? item.id : !!item.code ? item.code : index;
    }

    back() {
        window.history.back();
    }
}
