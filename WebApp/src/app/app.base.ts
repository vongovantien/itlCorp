import { OnInit } from "@angular/core";

export abstract class AppPage implements OnInit {
  constructor() {}

  ngOnInit(): void {}

  ngOnDestroy(): void {}

  ngDoCheck(): void {}

  ngOnChanges(changes: any): void {}

  ngAfterContentInit(): void {}

  ngAfterContentChecked(): void {}

  ngAfterViewInit(): void {}

  ngAfterViewChecked(): void {}

  trackByFn = (index: number, item: any): any => {
    return !!item.id ? item.id : !!item.code ? item.code : index;
  };

  back() {
    window.history.back();
  }
}
