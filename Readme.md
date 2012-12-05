Time Light
==========

Time Light keeps track of how much time you have spent on various projects.

Timing
------

The program's GUI is a timer icon in the notification area.
The timer is green when timing, otherwise red. Clicking the timer icon will start or stop the timer.

Right-clicking on the icon will bring up a menu of the project tree. Clicking on an element will select it as the active item, and begin timing.
If a different element was previously active, it will be stopped.

Ledger
------

The ledger is an XML file, containing a tree of projects and sub-projects. The root element must be `node`.
Nodes contain other nodes and `leaf`s. Each element (except the root) must be labelled with a `name` attribute.

Each time the timer is stopped, it will update the contents of the current leaf, as well as the `total` attribute in all of its ancestor nodes.
If a node has a `billed` attribute, this will also be updated.

The ledger will be automatically reloaded if it changes while the program is running.

Settings
--------

Logging can be configured in the settings file.
If the event log is used, the program must be run once as administrator to create the event source, or enter the following line from an elevated command prompt:

    eventcreate /id 1 /l application /t information /so "Time Light" /d "create event source"

The settings file also specifies the path to the ledger (relative to the user's documents folder), and the program used for viewing/editing.

Example ledger
--------------

    <node>
      <node name="Colors">
        <leaf name="Orange" />
        <node name="Primary">
          <leaf name="Red" />
          <leaf name="Green" />
        </node>
      </node>
      <leaf name="Shapes" />
    </node>