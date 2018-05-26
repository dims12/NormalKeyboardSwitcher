# Normal Keyboard Switcher

(Under development)

This simple program is a replacement for standard Windows keyboard switcher, and suitable for switching 
more than two input languages. Standard switcher makes this hard, because it does full circle of 
languages, until returns to initial one. If Windows is configured to remember language for each opened 
window (default), then you get dozens of input languages on your screen and current language
changes unpredictable. In order to set desired language, you need to press language switching
combintaion multiple times.

Normal keyboard switcher uses the same algorithm as task switcher does: while you are pressing switching
combination, you are going by the circle, but once you stopped, your last selected language goes
on top of the stack and previous one goes second. If you press switching combination again, you will
go to your previous language, not next one in the circle.

Normal keyboard switcher is a simple alternative to [Mahou](https://github.com/BladeMight/Mahou).

## To Do

1. Add key combinations other than Ctrl-Shift
5. Make icon
2. Turn into system tray application 
3. Make autorun
4. Make installer

## How to run



